package models

type Action struct {
	ID         int    `json:"id" gorm:"primaryKey;autoIncrement"`
	Difficulty int    `json:"difficulty"`
	LefSel     string `json:"lef_sel"`
	RigSel     string `json:"rig_sel"`
	Detail     string `json:"detail"`
}
