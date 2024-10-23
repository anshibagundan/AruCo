package models

import "gorm.io/gorm"

type User struct {
	gorm.Model
	ID   int    `json:"id"`
	UUID string `json:"uuid"`
	Code int    `json:"code"`
}
