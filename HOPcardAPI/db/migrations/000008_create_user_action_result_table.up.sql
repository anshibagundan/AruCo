CREATE TABLE user_action_results (
                                    id BIGSERIAL PRIMARY KEY,
                                    user_data_id INT NOT NULL,
                                    action_id INT NOT NULL,
                                    correct_rate FLOAT NOT NULL, -- 正解率を格納
                                    attempt_count INT DEFAULT 1,
                                    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                                    FOREIGN KEY (user_data_id) REFERENCES user_data(id) ON DELETE CASCADE
);
