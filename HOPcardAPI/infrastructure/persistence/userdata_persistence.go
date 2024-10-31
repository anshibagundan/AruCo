package persistence

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
	"gorm.io/gorm"
	"strconv"
)

type UserDataPersistence struct {
	db *gorm.DB
}

func NewUserDataPersistence(db *gorm.DB) repositories.UserDataRepository {
	return &UserDataPersistence{db}
}

func (p *UserDataPersistence) Save(uuid string, ratio float64, distance float64) (string, error) {
	var uuidRecord models.UUID
	if err := p.db.Where("uuid = ?", uuid).First(&uuidRecord).Error; err != nil {
		return "", err
	}

	userData := models.UserData{
		UuidID:   uuidRecord.ID,
		Ratio:    ratio,
		Distance: distance,
	}

	if err := p.db.Create(&userData).Error; err != nil {
		return uuid, err
	}

	return uuid, nil
}

func (p *UserDataPersistence) FindByUuid(uuid string) (*models.UserData, error) {
	var uuidRecord models.UUID
	if err := p.db.Where("uuid = ?", uuid).First(&uuidRecord).Error; err != nil {
		return nil, err
	}

	var userData models.UserData
	if err := p.db.Where("uuid_id = ?", uuidRecord.ID).First(&userData).Error; err != nil {
		return nil, err
	}

	return &userData, nil
}

func (p *UserDataPersistence) Update(userData *models.UserData) (string, error) {
	if err := p.db.Save(userData).Error; err != nil {
		return "", err
	}
	return strconv.Itoa(userData.UuidID), nil // uuidを返す
}
