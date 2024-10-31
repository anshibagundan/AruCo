package models

type ResultUnityMessage struct {
	QuizID   []int   `json:"quiz_id"`
	ActionID int     `json:"action_id"`
	Cor      []bool  `json:"cor"`
	Distance float64 `json:"distance"`
}

type ResultAndroidMessage struct {
	Cor      []bool  `json:"cor"`
	Distance float64 `json:"distance"`
	Message  string  `json:"message"`
}
