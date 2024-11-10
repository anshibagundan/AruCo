package repositories

import "HOPcardAPI/domain/models"

type ActionRepository interface {
	FindOneByDifficulty(difficulty int) (*models.Action, error)
	FindOneByID(id int) (*models.Action, error)
}
