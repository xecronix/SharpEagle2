PRAGMA foreign_keys = ON;

CREATE TABLE contacts (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL
);

CREATE TABLE addresses (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    contact_id INTEGER NOT NULL,
    street TEXT NOT NULL,
    city TEXT NOT NULL,
    zip TEXT NOT NULL,
    FOREIGN KEY (contact_id) REFERENCES contacts(id)
);

CREATE TABLE phones (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    contact_id INTEGER NOT NULL,
    phone TEXT NOT NULL,
    FOREIGN KEY (contact_id) REFERENCES contacts(id)
);