package models

type Quiz struct {
	ID         int    `json:"id" gorm:"primaryKey;autoIncrement"`
	Name       string `json:"name"`
	Difficulty int    `json:"difficulty"`
	LefSel     string `json:"lef_sel"`
	RigSel     string `json:"rig_sel"`
	Detail     string `json:"detail"`
}
