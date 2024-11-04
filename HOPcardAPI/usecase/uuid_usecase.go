package usecase

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"HOPcardAPI/domain/services"
	"math/rand"
)

type UUIDUseCase interface {
	CreateUUID() (*models.UUID, error)
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

func (uc *uuidUseCase) CreateUUID() (*models.UUID, error) {
	var code int

	// 一意な6桁のコードが生成されるまでループ
	for {
		// 6桁のランダムなコードを生成 (100000から999999の範囲)
		code = 100000 + rand.Intn(900000)

		// 生成したコードが既に存在するかを確認
		uuid, err := uc.uuidRepo.FindByCode(code)
		if err != nil {
			return nil, err
		}
		if uuid.Code != code {
			break // 一意なコードが見つかったらループを抜ける
		}
	}

	// UUIDを生成し、データベースに保存
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
