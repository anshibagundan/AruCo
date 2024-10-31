CREATE TABLE user_data (
                           id SERIAL PRIMARY KEY,
                           uuid_id INT REFERENCES uuids(id),
                           ratio FLOAT,
                           distance FLOAT,
                           change_count INT DEFAULT 0,
                           created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                           updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
