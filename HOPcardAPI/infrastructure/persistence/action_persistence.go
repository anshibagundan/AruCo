package persistence

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"gorm.io/gorm"
)

type actionRepository struct {
	db *gorm.DB
}

func NewActionRepository(db *gorm.DB) repositories.ActionRepository {
	return &actionRepository{db: db}
}

func (r *actionRepository) FindOneByDifficulty(difficulty int) (*models.Action, error) {
	var action models.Action
	err := r.db.Where("difficulty = ?", difficulty).First(&action).Error
	return &action, err
}
