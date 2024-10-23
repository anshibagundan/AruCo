package usecase

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"HOPcardAPI/domain/services"
)

type UUIDUseCase interface {
	CreateUUID(code int) (*models.UUID, error)
	GetUUIDByCode(code int) (*models.UUID, error)
}

type uuidUseCase struct {
	uuidRepo    repositories.UUIDRepository
	uuidService services.UUIDService
}

func NewUUIDUseCase(repo repositories.UUIDRepository, service services.UUIDService) UUIDUseCase {
	return &uuidUseCase{
		uuidRepo:    repo,
		uuidService: service,
	}
}

func (uc *uuidUseCase) CreateUUID(code int) (*models.UUID, error) {
	generatedUUID := uc.uuidService.GenerateUUID(code)
	uuid := &models.UUID{
		UUID: generatedUUID,
		Code: code,
	}

	err := uc.uuidRepo.Create(uuid)
	if err != nil {
		return nil, err
	}
	return uuid, nil
}

func (uc *uuidUseCase) GetUUIDByCode(code int) (*models.UUID, error) {
	return uc.uuidRepo.FindByCode(code)
}
