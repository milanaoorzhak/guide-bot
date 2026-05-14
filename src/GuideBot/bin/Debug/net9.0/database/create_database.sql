DO
$$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'admin') THEN
        CREATE ROLE admin WITH LOGIN PASSWORD 'admin1234!';
    END IF;
END
$$;

SELECT 'CREATE DATABASE tuva_guide_bot OWNER admin'
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'tuva_guide_bot')\gexec
