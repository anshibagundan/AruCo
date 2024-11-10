package repositories

import "HOPcardAPI/domain/models"

type UserActionResultRepository interface {
	Save(result models.UserActionResult) error
	Update(result *models.UserActionResult) error
	FindByUserDataID(user_data_id int) ([]models.UserActionResult, error)
	FindByUserDataAndActionID(user_data_id int, actionID int) (*models.UserActionResult, error)
}
