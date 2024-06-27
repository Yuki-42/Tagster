/*
LANG=sqlite3

WARNING: DO NOT ATTEMPT TO EDIT THIS FILE UNLESS YOU KNOW WHAT YOU ARE DOING!

This is the schema file for the FileMgr database. It is used to create the tables and relationships between them.

This file is executed as a SQL script, so you can use any valid SQL commands in here.
*/

CREATE TABLE IF NOT EXISTS files (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    added DATETIME DEFAULT CURRENT_TIMESTAMP,
    path TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS tags (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    created DATETIME DEFAULT CURRENT_TIMESTAMP,
    name TEXT NOT NULL UNIQUE,
    colour TEXT
);

CREATE TABLE IF NOT EXISTS file_tags (
    file_id INTEGER NOT NULL,
    tag_id INTEGER NOT NULL,
    PRIMARY KEY (file_id, tag_id),
    FOREIGN KEY (file_id) REFERENCES files (id) ON DELETE CASCADE,
    FOREIGN KEY (tag_id) REFERENCES tags (id) ON DELETE CASCADE
);
