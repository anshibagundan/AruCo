package persistence

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"gorm.io/gorm"
)

type quizRepository struct {
	db *gorm.DB
}

func NewQuizRepository(db *gorm.DB) repositories.QuizRepository {
	return &quizRepository{db: db}
}

func (r *quizRepository) FindByDifficulty(difficulty int, limit int) ([]models.Quiz, error) {
	var quizzes []models.Quiz
	err := r.db.Where("difficulty = ?", difficulty).Order("RANDOM()").Limit(limit).Find(&quizzes).Error
	return quizzes, err
}

func (r *quizRepository) FindByID(id int) (*models.Quiz, error) {
	var quiz models.Quiz
	err := r.db.Where("id = ?", id).First(&quiz).Error
	return &quiz, err
}
