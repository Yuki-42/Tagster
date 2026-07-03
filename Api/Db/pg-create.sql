/* This file is used to create the database and users for the Tagster API. It should be run as a superuser in PostgreSQL. */
CREATE USER "Tagster-API-Audit" WITH PASSWORD '';
CREATE USER "Tagster-API-Main" WITH PASSWORD '';

/* Create the database and set the owner to Tagster-API-Audit, which will have full permissions on the database. Tagster-API-Main will have limited permissions. */
CREATE DATABASE tagster OWNER "Tagster-API-Audit";

/* Include uuid-ossp extension for generating UUIDs. */
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

/* Audit schema must only be select-insert for Tagster-API-Main. Tagser-API-Audit should have full permissions on the audit schema. */
CREATE SCHEMA audit;
CREATE SCHEMA protected;
CREATE SCHEMA ingests;

/* Allow Tagster-API-Main to connect to the database and read/write to the tables it needs to access, but not modify the database schema or access other databases on the server. */
GRANT CONNECT ON DATABASE tagster TO "Tagster-API-Main";

GRANT USAGE ON SCHEMA public TO "Tagster-API-Main";
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO "Tagster-API-Main";
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO "Tagster-API-Main";

GRANT USAGE ON SCHEMA audit TO "Tagster-API-Main";
GRANT SELECT, INSERT ON ALL TABLES IN SCHEMA audit TO "Tagster-API-Main";
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA audit TO "Tagster-API-Main";

GRANT USAGE ON SCHEMA protected TO "Tagster-API-Main";
GRANT SELECT, UPDATE ON ALL TABLES IN SCHEMA protected TO "Tagster-API-Main";
GRANT SELECT, USAGE ON ALL SEQUENCES IN SCHEMA protected TO "Tagster-API-Main";

GRANT USAGE ON SCHEMA ingests TO "Tagster-API-Main";
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA ingests TO "Tagster-API-Main";
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA ingests TO "Tagster-API-Main";

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
    previous_state text,
    effected       bool,
    comment        text
);

CREATE TABLE protected.environment
(
    key   text  NOT NULL PRIMARY KEY,
    dtype INT   NOT NULL,
    data  bytea NOT NULL
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
    signature     text      NOT NULL,
    user_id       uuid      NOT NULL,
    issued        timestamp NOT NULL,
    expires       timestamp NOT NULL,
    permissions   int       NOT NULL,
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

CREATE TABLE public.media
(
    id            uuid NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    media_type    int  NOT NULL,
    captured      DATE NOT NULL,
    time_captured TIME,
    rating        int,
    original_name text NOT NULL,
    width         int  NOT NULL,
    height        int  NOT NULL,
    file_size     int  NOT NULL
);

CREATE TABLE public.media_tags (
    id   uuid NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    media_id uuid NOT NULL,
    tag_id uuid NOT NULL
);

CREATE TABLE ingests.sessions
(
    id   uuid NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    name text NOT NULL UNIQUE
);

CREATE TABLE ingests.ingest_media_items
(
    id         uuid NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    media_id   uuid NOT NULL,
    session_id uuid NOT NULL,
    exists     bool NOT NULL             DEFAULT FALSE,
    FOREIGN KEY (media_id) REFERENCES public.media (id) ON DELETE CASCADE,
    FOREIGN KEY (session_id) REFERENCES ingests.sessions (id) ON DELETE CASCADE
);

CREATE TABLE ingests.ingest_tags
(
    id         uuid NOT NULL PRIMARY KEY DEFAULT uuid_generate_v4(),
    tag_id     uuid NOT NULL,
    session_id uuid NOT NULL,
    FOREIGN KEY (tag_id) REFERENCES public.tags (id) ON DELETE CASCADE,
    FOREIGN KEY (session_id) REFERENCES ingests.sessions (id) ON DELETE CASCADE
);

/* Currently unused, but intended for queue of media processing jobs for resizing and thumbnail generation */
CREATE TABLE ingests.media_processing_queue
(

);


