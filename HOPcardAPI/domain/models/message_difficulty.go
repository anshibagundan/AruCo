package models

type AndroidDifficultyMessage struct {
	Difficulty int `json:"difficulty"`
}

type UnityDifficultyMessage struct {
	QuizIDs  []int `json:"quiz_id"`
	ActionID int   `json:"action_id"`
}
