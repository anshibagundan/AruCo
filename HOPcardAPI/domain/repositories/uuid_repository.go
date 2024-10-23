package repositories

import "HOPcardAPI/domain/models"

type UUIDRepository interface {
	Create(uuid *models.UUID) error
	FindByCode(code int) (*models.UUID, error)
}
