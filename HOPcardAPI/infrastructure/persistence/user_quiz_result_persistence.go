package persistence

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"gorm.io/gorm"
)

type userquizresultRepository struct {
	db *gorm.DB
}

func NewUserQuizResultRepository(db *gorm.DB) repositories.UserQuizResultRepository {
	return &userquizresultRepository{db: db}
}

// FindByUserDataID は、特定のUserDataIDに関連するクイズ結果を取得
func (u userquizresultRepository) FindByUserDataID(user_data_id int) ([]models.UserQuizResult, error) {
	var userquizresults []models.UserQuizResult
	err := u.db.Where("user_data_id = ?", user_data_id).Find(&userquizresults).Error
	if err != nil {
		return nil, err
	}
	return userquizresults, nil
}

// FindByUserDataAndQuizID は、特定のUserDataIDとQuizIDに基づいてクイズ結果を取得
func (u userquizresultRepository) FindByUserDataAndQuizID(user_data_id int, quizID int) (*models.UserQuizResult, error) {
	var result models.UserQuizResult
	err := u.db.Where("user_data_id = ? AND quiz_id = ?", user_data_id, quizID).First(&result).Error
	if err != nil {
		if err == gorm.ErrRecordNotFound {
			return nil, nil // レコードが存在しない場合はnilを返す
		}
		return nil, err // その他のエラーを返す
	}
	return &result, nil
}

// Save は新しいクイズ結果をデータベースに挿入
func (u userquizresultRepository) Save(result models.UserQuizResult) error {
	return u.db.Create(&result).Error
}

// Update は既存のクイズ結果を更新
func (u userquizresultRepository) Update(result *models.UserQuizResult) error {
	return u.db.Save(result).Error
}
