# Raspberry Pi – Einrichtung für „nur noch einschalten"

Ziel: Nach dieser **einmaligen** Einrichtung musst du den Raspberry Pi nur noch
**einschalten**. Er holt dann automatisch den neuesten Stand aus Git, baut die
Docker-Images (nur wenn sich etwas geändert hat) und startet das Bewirtungssystem.

> Die Container laufen dank `restart: unless-stopped` in `docker-compose.yml`
> ohnehin nach jedem Neustart automatisch wieder an. Der hier eingerichtete
> systemd-Dienst sorgt zusätzlich dafür, dass beim Hochfahren der **aktuelle
> Code** geholt und neu gebaut wird.

## Voraussetzungen (einmalig)

1. **Docker + Docker Compose** installiert:
   ```bash
   curl -fsSL https://get.docker.com | sh
   sudo usermod -aG docker $USER   # danach einmal ab- und wieder anmelden
   ```

2. **Repository klonen** (Beispiel für Benutzer `pi`):
   ```bash
   cd ~
   git clone https://github.com/Kech2025/Bewirtungssystem.git
   cd Bewirtungssystem
   ```

3. **.env anlegen** (falls noch nicht vorhanden) und Werte eintragen:
   ```bash
   cp .env.example .env
   nano .env
   ```
   - `BEWIRTUNG_DB_CONNECTION` → Passwort der bestehenden Datenbank eintragen.
   - `STIRLING_PDF_BASEURL` → URL/IP der laufenden Stirling-PDF-Instanz.
   - `WEB_HOST_PORT` → Port der Weboberfläche (Standard 8080).

## Autostart einrichten (einmalig)

1. Skript ausführbar machen:
   ```bash
   chmod +x ~/Bewirtungssystem/scripts/raspberry-autostart.sh
   ```

2. systemd-Dienst installieren. **Wichtig:** Falls dein Benutzer nicht `pi`
   heißt oder das Repo woanders liegt, zuerst die Pfade/den `User` in
   `scripts/bewirtung.service` anpassen.
   ```bash
   sudo cp ~/Bewirtungssystem/scripts/bewirtung.service /etc/systemd/system/bewirtung.service
   sudo systemctl daemon-reload
   sudo systemctl enable bewirtung.service
   ```

3. Testen (ohne Neustart):
   ```bash
   sudo systemctl start bewirtung.service
   sudo systemctl status bewirtung.service
   docker compose ps
   ```

## Ab jetzt

- **Raspberry Pi einschalten → fertig.** Das System aktualisiert sich und startet
  automatisch.
- Weboberfläche: `http://<Raspberry-IP>:8080` (bzw. dein `WEB_HOST_PORT`).

## Nützliche Befehle

| Zweck | Befehl |
|-------|--------|
| Status ansehen | `docker compose ps` |
| Logs ansehen | `docker compose logs -f` |
| Manuell aktualisieren | `cd ~/Bewirtungssystem && git pull && docker compose up -d --build` |
| Dienst-Logs | `journalctl -u bewirtung.service -f` |
| Autostart deaktivieren | `sudo systemctl disable bewirtung.service` |
