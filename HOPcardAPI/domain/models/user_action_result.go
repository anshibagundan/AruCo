package models

import "time"

type UserActionResult struct {
	ID           uint      `json:"id" gorm:"primaryKey;autoIncrement"`
	UserDataID   uint      `json:"user_data_id"`
	ActionID     int       `json:"action_id"`
	CorrectRate  float64   `json:"correct_rate"`
	AttemptCount int       `json:"attempt_count"`
	CreatedAt    time.Time `json:"created_at" gorm:"autoCreateTime"`
}
