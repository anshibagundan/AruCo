CREATE TABLE uuids (
                       id SERIAL PRIMARY KEY,
                       uuid VARCHAR(36) NOT NULL,
                       code INTEGER NOT NULL,
                       created_at TIMESTAMP WITH TIME ZONE,
                       updated_at TIMESTAMP WITH TIME ZONE,
                       deleted_at TIMESTAMP WITH TIME ZONE
);