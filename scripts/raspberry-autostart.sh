#!/usr/bin/env bash
#
# Autostart-Skript für das Bewirtungssystem auf dem Raspberry Pi.
#
# Wird beim Hochfahren über den systemd-Dienst "bewirtung.service" ausgeführt.
# Es holt den aktuellen Stand aus Git und startet die Container neu.
# Dank "restart: unless-stopped" in der docker-compose.yml laufen die Container
# danach auch nach jedem Neustart automatisch wieder an.
#
# Einmalige Einrichtung: siehe RASPBERRY-SETUP.md
#
set -euo pipefail

# Verzeichnis, in dem das Repository liegt (ggf. anpassen).
PROJEKT_VERZEICHNIS="${BEWIRTUNG_DIR:-$HOME/Bewirtungssystem}"

cd "$PROJEKT_VERZEICHNIS"

echo "[$(date '+%Y-%m-%d %H:%M:%S')] Bewirtungssystem wird gestartet..."

# Auf Netzwerk/Internet warten (max. 60 Sekunden), damit git pull gelingt.
for i in $(seq 1 30); do
	if ping -c1 -W2 github.com >/dev/null 2>&1; then
		break
	fi
	echo "Warte auf Netzwerk... ($i)"
	sleep 2
done

# Neuesten Stand holen (schlägt nicht fehl, wenn kein Internet verfügbar ist).
if ping -c1 -W2 github.com >/dev/null 2>&1; then
	echo "Hole aktuellen Stand aus Git..."
	git pull --ff-only || echo "git pull fehlgeschlagen - fahre mit lokalem Stand fort."
else
	echo "Kein Internet - starte mit lokalem Stand."
fi

# Container bauen (falls sich Code geändert hat) und im Hintergrund starten.
echo "Starte Docker-Container..."
docker compose up -d --build

echo "[$(date '+%Y-%m-%d %H:%M:%S')] Bewirtungssystem läuft."
