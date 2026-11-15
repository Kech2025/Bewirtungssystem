# Hochverfügbare PostgreSQL mit CloudNativePG (CNPG)

Diese Anleitung ersetzt die bisherige Einzel-Instanz
(`postgres-postgresql-0`, Bitnami-Chart, 1 Pod auf `pi-1`) durch einen
**hochverfügbaren PostgreSQL-Cluster** mit automatischem Failover.

## Warum CloudNativePG?

- **3 Instanzen** (1 Primary + 2 Replicas), je eine pro Raspberry Pi.
- **Streaming-Replikation** zwischen den Instanzen.
- **Automatisches Failover**: Fällt der Primary-Node aus, wird ein Replica
  automatisch zum neuen Primary befördert.
- **Kein verteilter Storage nötig** – jede Instanz nutzt ihr eigenes
  lokales `local-path`-Volume auf ihrem Node.

## Wichtige Services nach dem Deploy

CNPG legt automatisch Services an:

- `bewirtung-pg-rw`  → **immer** der aktuelle Primary (read/write)
- `bewirtung-pg-ro`  → nur Replicas (read-only)
- `bewirtung-pg-r`   → beliebige Instanz (read)

Die Anwendung muss den **`-rw`-Service** verwenden, damit sie nach einem
Failover automatisch dem neuen Primary folgt.

---

## 1. CNPG-Operator installieren

```sh
kubectl apply --server-side -f \
  https://raw.githubusercontent.com/cloudnative-pg/cloudnative-pg/release-1.24/releases/cnpg-1.24.1.yaml

# Warten, bis der Operator läuft:
kubectl rollout status deployment -n cnpg-system cnpg-controller-manager
```

> Prüfe die jeweils aktuelle stabile Version auf
> https://github.com/cloudnative-pg/cloudnative-pg/releases

## 2. Secret und Cluster anlegen

```sh
kubectl apply -f 05-postgres-secret.yaml
kubectl apply -f 10-postgres-cluster.yaml

# Fortschritt beobachten (dauert ein paar Minuten auf dem Pi):
kubectl get pods -n database -w
kubectl get cluster -n database
```

Erwartet: `bewirtung-pg-1/-2/-3` jeweils `Running`, verteilt auf pi-1/2/3:

```sh
kubectl get pods -n database -o wide
```

---

## 3. Daten aus der alten DB übernehmen (pg_dump/restore)

Kurze Downtime: Während des Dumps sollten keine neuen Bestellungen laufen.

### 3a. Dump aus der alten Bitnami-DB ziehen

```sh
# In den alten Postgres-Pod exec'en und Dump erzeugen:
kubectl exec -n database postgres-postgresql-0 -- \
  env PGPASSWORD=Kiwo2014 pg_dump -U postgres -d bewirtung -Fc -f /tmp/bewirtung.dump

# Dump auf den Arbeitsrechner/Pi kopieren:
kubectl cp database/postgres-postgresql-0:/tmp/bewirtung.dump ./bewirtung.dump
```

### 3b. Dump in den neuen CNPG-Primary einspielen

```sh
# Dump in den neuen Primary-Pod kopieren:
kubectl cp ./bewirtung.dump database/bewirtung-pg-1:/tmp/bewirtung.dump

# Restore ausführen (bewirtung-pg-1 = Primary beim ersten Start):
kubectl exec -n database bewirtung-pg-1 -- \
  env PGPASSWORD=Kiwo2014 pg_restore -U postgres -d bewirtung --clean --if-exists /tmp/bewirtung.dump
```

> Falls `bewirtung-pg-1` nicht der Primary ist, den Primary ermitteln:
> ```sh
> kubectl get cluster bewirtung-pg -n database -o jsonpath='{.status.currentPrimary}'
> ```

### 3c. Daten prüfen

```sh
kubectl exec -n database bewirtung-pg-1 -- \
  env PGPASSWORD=Kiwo2014 psql -U postgres -d bewirtung -c "\dt"
```

---

## 4. Anwendung auf den neuen Service umstellen

Das Secret `bewirtung-db` im Namespace `bewirtung` muss auf den
`-rw`-Service des CNPG-Clusters zeigen:

```
Host=bewirtung-pg-rw.database.svc.cluster.local;Port=5432;Database=bewirtung;Username=postgres;Password=Kiwo2014
```

Siehe `k3s/01-db-secret.yaml` – dort den Host anpassen und anwenden, danach
API und Webserver neu ausrollen:

```sh
kubectl apply -f ../01-db-secret.yaml
kubectl rollout restart deployment/bewirtung-api -n bewirtung
kubectl rollout restart deployment/bewirtung-webserver -n bewirtung
```

---

## 5. Alte Bitnami-DB abbauen (erst nach erfolgreicher Prüfung!)

```sh
# Wenn per Helm installiert:
helm list -n database
helm uninstall postgres -n database

# PVC der alten DB prüfen und ggf. entfernen:
kubectl get pvc -n database
```

---

## 6. HA testen (Failover)

```sh
# Aktuellen Primary ermitteln:
kubectl get cluster bewirtung-pg -n database -o jsonpath='{.status.currentPrimary}'; echo

# Den Primary-Pod löschen (simuliert Ausfall):
kubectl delete pod <primary-pod> -n database

# CNPG befördert automatisch einen Replica. Beobachten:
kubectl get cluster bewirtung-pg -n database -w

# App bleibt über -rw-Service erreichbar:
curl -i http://192.168.170.30:30580/api/benutzer
```

Erwartung: Nach wenigen Sekunden zeigt `currentPrimary` einen anderen Pod,
und die API liefert weiterhin Daten.
