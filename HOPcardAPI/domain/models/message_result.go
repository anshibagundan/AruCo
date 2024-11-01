package models

type ResultUnityMessage struct {
	QuizID   []int   `json:"quiz_id"`
	ActionID int     `json:"action_id"`
	Cor      []bool  `json:"cor"`
	Distance float64 `json:"distance"`
}

type ResultAndroidMessage struct {
	QuizNames  []string `json:"quiz_names"`
	Difficulty string   `json:"difficulty"`
	Cor        []bool   `json:"cor"`
	Distance   float64  `json:"distance"`
	Message    string   `json:"message"`
}
