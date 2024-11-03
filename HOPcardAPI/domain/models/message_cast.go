package models

import "encoding/json"

type CastMessage struct {
	Type    string          `json:"type"`
	Payload json.RawMessage `json:"payload"`
}
