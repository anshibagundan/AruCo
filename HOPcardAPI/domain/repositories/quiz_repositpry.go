package repositories

import (
	"HOPcardAPI/domain/models"
)

type QuizRepository interface {
	FindByDifficulty(difficulty int, limit int) ([]models.Quiz, error)
	FindByID(id int) (models.Quiz, error)
}
