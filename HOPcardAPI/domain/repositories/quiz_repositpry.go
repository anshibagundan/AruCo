package repositories

import "HOPcardAPI/domain/models"

type QuizRepository interface {
	FindByDifficulty(difficulty int, limit int) ([]models.Quiz, error)
}
