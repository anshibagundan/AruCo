package gpt

import (
	"bytes"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"
)

const url = "https://api.openai.com/v1/chat/completions"

func AskChatGPT(prompt string) (string, error) {
	log.Printf("ChatGPTに問い合わせ中: %s", prompt)
	apiKey := os.Getenv("OPENAI_API_KEY")
	requestBody := map[string]interface{}{
		"model": "gpt-3.5-turbo",
		"messages": []map[string]string{
			{"role": "user", "content": prompt},
		},
		"max_tokens": 300,
	}

	jsonData, err := json.Marshal(requestBody)
	if err != nil {
		return "", fmt.Errorf("リクエストボディのマーシャルに失敗しました: %v", err)
	}

	req, err := http.NewRequest("POST", url, bytes.NewBuffer(jsonData))
	if err != nil {
		return "", fmt.Errorf("リクエストの作成に失敗しました: %v", err)
	}
	req.Header.Set("Authorization", "Bearer "+apiKey)
	req.Header.Set("Content-Type", "application/json")

	client := &http.Client{Timeout: 10 * time.Second}
	resp, err := client.Do(req)
	if err != nil {
		return "", fmt.Errorf("リクエストの実行に失敗しました: %v", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", fmt.Errorf("リクエストが失敗しました: %s", resp.Status)
	}

	var responseBody struct {
		Choices []struct {
			Message struct {
				Content string `json:"content"`
			} `json:"message"`
		} `json:"choices"`
	}

	if err := json.NewDecoder(resp.Body).Decode(&responseBody); err != nil {
		return "", fmt.Errorf("レスポンスのデコードに失敗しました: %v", err)
	}

	if len(responseBody.Choices) == 0 {
		return "", fmt.Errorf("レスポンスにチョイスがありません")
	}

	return responseBody.Choices[0].Message.Content, nil
}
