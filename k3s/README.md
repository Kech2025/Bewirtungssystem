# Bewirtung – k3s-Deployment

Manifeste zum Betrieb von **Bewirtung.Api** und **Bewirtung.Webserver** im k3s-Cluster.

## Voraussetzungen

- Docker-Images liegen öffentlich auf Docker Hub:
  - `kechg/bewirtung-api:latest`
  - `kechg/bewirtung-webserver:latest`
- PostgreSQL läuft im Cluster als Service `postgres-external` im Namespace `postgres`.
- Der USB-Stick ist auf dem gewünschten Node unter `/media/usb` gemountet
  und der Node ist gelabelt:

  ```sh
  kubectl label node <NODE-NAME> usb-stick=true
  ```

## DB-Passwort setzen

Vor dem Anwenden ggf. das Passwort in `01-db-secret.yaml` anpassen.

## Deployen

```sh
kubectl apply -f 00-namespace.yaml
kubectl apply -f 01-db-secret.yaml
kubectl apply -f 10-api.yaml
kubectl apply -f 20-webserver.yaml
```

oder alles auf einmal:

```sh
kubectl apply -f .
```

## Prüfen

```sh
# Laufen die Pods? Auf welchem Node?
kubectl get pods -n bewirtung -o wide

# Sieht der Webserver-Pod den USB-Stick?
kubectl exec -n bewirtung deploy/bewirtung-webserver -- ls -la /media/usb
```

## Zugriff (NodePort)

- Webserver:  `http://<NODE-IP>:30080`
- API:        `http://<NODE-IP>:30580`

## Updates ausrollen

Nach einem neuen Image-Push (gleiches `:latest`-Tag):

```sh
kubectl rollout restart deployment/bewirtung-webserver -n bewirtung
kubectl rollout restart deployment/bewirtung-api -n bewirtung
```

## Hinweis USB-Export

Der Button „Auf USB-Stick speichern" im DSFinV-K-Export schreibt serverseitig
nach `/media/usb` (konfigurierbar über `Export__UsbZielPfad`). Das funktioniert
nur, wenn der Pod auf dem gelabelten Node läuft **und** dort der Stick unter
`/media/usb` gemountet ist. Andernfalls erscheint die Fehlermeldung, dass das
Zielverzeichnis nicht gefunden wurde.
