package models

import (
	"time"
)

type UserData struct {
	ID          uint      `json:"id" gorm:"primaryKey;autoIncrement"`
	UuidID      int       `json:"uuid_id" gorm:"foreignKey:UuidID;references:id"`
	Ratio       float64   `json:"ratio"`
	Distance    float64   `json:"distance"`
	ChangeCount int       `json:"change_count" gorm:"default:0"`
	CreatedAt   time.Time `json:"created_at" gorm:"autoCreateTime"`
	UpdatedAt   time.Time `json:"updated_at" gorm:"autoUpdateTime"`
}
