package repositories

import "HOPcardAPI/domain/models"

type UserDataRepository interface {
	Save(uuid string, ratio float64, distance float64) (string, error)
	FindByUuid(uuid string) (*models.UserData, error)
	Update(userData *models.UserData) (string, error)
}
