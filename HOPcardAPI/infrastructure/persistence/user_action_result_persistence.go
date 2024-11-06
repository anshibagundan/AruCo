package persistence

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"gorm.io/gorm"
)

type useractionresultRepository struct {
	db *gorm.DB
}

func NewUserActionResultRepository(db *gorm.DB) repositories.UserActionResultRepository {
	return &useractionresultRepository{db: db}
}

// FindByUserDataID は、特定のUserDataIDに関連するクイズ結果を取得
func (u useractionresultRepository) FindByUserDataID(user_data_id int) ([]models.UserActionResult, error) {
	var useractionresults []models.UserActionResult
	err := u.db.Where("user_data_id = ?", user_data_id).Find(&useractionresults).Error
	if err != nil {
		return nil, err
	}
	return useractionresults, nil
}

// FindByUserDataAndQuizID は、特定のUserDataIDとQuizIDに基づいてクイズ結果を取得
func (u useractionresultRepository) FindByUserDataAndActionID(user_data_id int, actionID int) (*models.UserActionResult, error) {
	var result models.UserActionResult
	err := u.db.Where("user_data_id = ? AND action_id = ?", user_data_id, actionID).First(&result).Error
	if err != nil {
		if err == gorm.ErrRecordNotFound {
			return nil, nil
		}
		return nil, err
	}
	return &result, nil
}

// Save は新しいクイズ結果をデータベースに挿入
func (u useractionresultRepository) Save(result models.UserActionResult) error {
	return u.db.Create(&result).Error
}

// Update は既存のクイズ結果を更新
func (u useractionresultRepository) Update(result *models.UserActionResult) error {
	return u.db.Save(result).Error
}
