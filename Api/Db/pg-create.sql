/* This file is used to create the database and users for the Tagster API. It should be run as a superuser in PostgreSQL. */
CREATE USER "Tagster-API-Audit" WITH PASSWORD '';
CREATE USER "Tagster-API-Main" WITH PASSWORD '';

/* Create the database and set the owner to Tagster-API-Audit, which will have full permissions on the database. Tagster-API-Main will have limited permissions. */
CREATE DATABASE "Tagster" OWNER "Tagster-API-Audit";

/* Include uuid-ossp extension for generating UUIDs. */
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

/* Audit schema must only be select-insert for Tagster-API-Main. Tagser-API-Audit should have full permissions on the audit schema. */
CREATE SCHEMA audit;

/* Allow Tagster-API-Main to connect to the database and read/write to the tables it needs to access, but not modify the database schema or access other databases on the server. */
GRANT CONNECT ON DATABASE tagster TO "Tagster-API-Main";

GRANT USAGE ON SCHEMA public TO "Tagster-API-Main";
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO "Tagster-API-Main";
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO "Tagster-API-Main";

GRANT USAGE ON SCHEMA audit TO "Tagster-API-Main";
GRANT SELECT, INSERT ON ALL TABLES IN SCHEMA audit TO "Tagster-API-Main";
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA audit TO "Tagster-API-Main";

/* Note: Don't need to grant permissions to Tagster-API-Audit as it is the owner of the database and has full permissions by default. */

/* Tables now (subject to change as we develop the API) */
CREATE TABLE audit.log
(
    id             uuid      NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    timestamp      timestamp NOT NULL,
    table_name     text      NOT NULL,
    action_type    int       NOT NULL,
    row_id         uuid      NOT NULL,
    user_id        uuid,
    previous_state jsonb,
    effected       bool,
    comment        text
);

CREATE TABLE public.users
(
    id       uuid NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    username TEXT NOT NULL,
    email    TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL
);

CREATE TABLE public.api_keys
(
    id            uuid      NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    key_value     text      NOT NULL,
    user_id       uuid      NOT NULL,
    issued        timestamp NOT NULL,
    expires       timestamp NOT NULL,
    user_agent    text      NOT NULL,
    ip_address    text      NOT NULL,
    friendly_name text,
    is_active     bool      NOT NULL             DEFAULT TRUE,
    FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
);

CREATE TABLE public.tags
(
    id          uuid NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        text NOT NULL,
    description text,
    colour      text
);
    


