package repositories

import "HOPcardAPI/domain/models"

type UserQuizResultRepository interface {
	Save(result models.UserQuizResult) error
	Update(result *models.UserQuizResult) error
	FindByUserDataID(user_data_id int) ([]models.UserQuizResult, error)
	FindByUserDataAndQuizID(user_data_id int, quizID int) (*models.UserQuizResult, error)
}
