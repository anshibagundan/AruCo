package persistence

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"errors"
	"gorm.io/gorm"
)

type uuidRepository struct {
	db *gorm.DB
}

func NewUUIDRepository(db *gorm.DB) repositories.UUIDRepository {

	return &uuidRepository{db: db}
}

// 既存のコードが存在するかチェックするメソッド
func (r *uuidRepository) isCodeExists(code int) bool {
	var count int64
	r.db.Model(&models.UUID{}).Where("code = ?", code).Count(&count)
	return count > 0
}

func (r *uuidRepository) Create(uuid *models.UUID) error {
	// トランザクションを開始
	return r.db.Transaction(func(tx *gorm.DB) error {
		// 既存のコードをチェック
		if r.isCodeExists(uuid.Code) {
			return errors.New("code already exists")
		}

		// codeをもとにuuidを新規作成
		return tx.Create(uuid).Error
	})
}

func (r *uuidRepository) FindByCode(code int) (*models.UUID, error) {
	var uuid models.UUID
	err := r.db.Where("code = ?", code).First(&uuid).Error
	if err != nil {
		return nil, err
	}
	return &uuid, nil
}
