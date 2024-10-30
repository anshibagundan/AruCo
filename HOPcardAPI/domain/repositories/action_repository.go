package repositories

import "HOPcardAPI/domain/models"

type ActionRepository interface {
	FindOneByDifficulty(difficulty int) (*models.Action, error)
}
