CREATE TABLE user_data (
                           id BIGSERIAL PRIMARY KEY,
                           uuid_id INT NOT NULL,
                           ratio FLOAT NOT NULL,
                           distance FLOAT NOT NULL,
                           change_count INT DEFAULT 0,
                           created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                           updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);
