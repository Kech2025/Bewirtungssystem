-- Konfiguration der Cloud-TSE (fiskaly) – je eine Zeile für Test und Live.
-- API-Key und API-Secret werden verschlüsselt (AES-GCM, Master-Key CloudTse__MasterKey) gespeichert.
-- Genau eine Umgebung kann aktiv sein.
CREATE TABLE IF NOT EXISTS public.fiskal_tse_konfiguration (
	umgebung        varchar(10)  PRIMARY KEY CHECK (umgebung IN ('test', 'live')),
	api_key_enc     text         NOT NULL DEFAULT '',
	api_secret_enc  text         NOT NULL DEFAULT '',
	tss_id          varchar(64)  NOT NULL DEFAULT '',
	client_id       varchar(64)  NOT NULL DEFAULT '',
	aktiv           boolean      NOT NULL DEFAULT false,
	geaendert_am    timestamptz  NOT NULL DEFAULT now(),
	geaendert_von   varchar(200) NOT NULL DEFAULT ''
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_fiskal_tse_konfiguration_aktiv
	ON public.fiskal_tse_konfiguration (aktiv) WHERE aktiv;

INSERT INTO public.fiskal_tse_konfiguration (umgebung) VALUES ('test'), ('live')
ON CONFLICT (umgebung) DO NOTHING;
