package main

import (
	"HOPcardAPI/domain/services"
	"HOPcardAPI/infrastructure/persistence"
	"HOPcardAPI/interfaces/handlers"
	"HOPcardAPI/usecase"
	"fmt"
	"github.com/gorilla/mux"
	"github.com/joho/godotenv"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"log"
	"net/http"
	"os"
)

func main() {
	// データベース接続の初期化
	db, err := initDB()
	if err != nil {
		log.Fatal(err)
	}

	// UUID系の初期化
	uuidRepo := persistence.NewUUIDRepository(db)
	uuidService := services.NewUUIDService()
	uuidUseCase := usecase.NewUUIDUseCase(uuidRepo, uuidService)
	uuidHandler := handlers.NewUUIDHandler(uuidUseCase)

	// Quiz, Action系の初期化
	quizRepo := persistence.NewQuizRepository(db)
	actionRepo := persistence.NewActionRepository(db)
	quizUsecase := usecase.NewQuizUseCase(quizRepo)
	actionUsecase := usecase.NewActionUseCase(actionRepo)
	quizHandler := handlers.NewQuizHandler(quizUsecase)
	actionHandler := handlers.NewActionHandler(actionUsecase)

	// Difficulty系の初期化
	difficultyUsecase := usecase.NewDifficultyUsecase(quizRepo, actionRepo)
	difficultyHandler := handlers.NewDifficultyWebSocketHandler(difficultyUsecase)

	// XYZ系の初期化
	xyzHandler := handlers.XYZNewWebSocketHandler()

	// UserData系の初期化
	userdataRepo := persistence.NewUserDataPersistence(db)
	userdataUseCase := usecase.NewUserDataUsecase(userdataRepo)
	userdataHandler := handlers.NewUserDataHandler(*userdataUseCase)

	// Result系の初期化
	resultHandler := handlers.ResultNewWebSocketHandler(quizRepo, actionRepo, userdataRepo, userdataUseCase)

	// ルーティング
	r := mux.NewRouter()
	r.HandleFunc("/createuuid", uuidHandler.CreateUUID).Methods("GET")
	r.HandleFunc("/getuuid", uuidHandler.GetUUID).Methods("GET")
	r.HandleFunc("/ws/difficulty/android/{uuid}", difficultyHandler.HandleAndroidWebSocket)
	r.HandleFunc("/ws/difficulty/unity/{uuid}", difficultyHandler.HandleUnityWebSocket)
	r.HandleFunc("/ws/xyz/android/{uuid}", xyzHandler.HandleXYZAndroidWebSocket)
	r.HandleFunc("/ws/xyz/unity/{uuid}", xyzHandler.HandleXYZUnityWebSocket)
	r.HandleFunc("/createuserdata", userdataHandler.CreateUserData).Methods("POST")
	r.HandleFunc("/getuserdata", userdataHandler.GetUserData).Methods("GET")
	r.HandleFunc("/ws/result/android/{uuid}", resultHandler.HandleResultAndroidWebSocket)
	r.HandleFunc("/ws/result/unity/{uuid}", resultHandler.HandleResultUnityWebSocket)
	r.HandleFunc("/getquiz", quizHandler.GetQuiz).Methods("GET")
	r.HandleFunc("/getaction", actionHandler.GetAction).Methods("GET")

	// ポート設定
	port := os.Getenv("PORT")
	if port == "" {
		port = "8080" // デフォルトポート
	}

	log.Printf("Starting server on port %s", port)
	log.Fatal(http.ListenAndServe(":"+port, r))
}

// initDBは別ファイルの方がいいのかな\(´ω` \)
func initDB() (*gorm.DB, error) {
	// .envファイルの読み込み
	if err := godotenv.Load(); err != nil {
		log.Printf("Warning: .env file not found")
	}

	// 環境変数から接続情報を取得
	dbURL := os.Getenv("DATABASE_URL")
	if dbURL == "" {
		return nil, fmt.Errorf("DATABASE_URL is not set")
	}

	log.Printf("Connecting to database with URL: %s", dbURL)

	// データベースに接続
	db, err := gorm.Open(postgres.Open(dbURL), &gorm.Config{})
	if err != nil {
		return nil, fmt.Errorf("failed to connect database: %w", err)
	}
	log.Printf("Connected to database")

	return db, nil
}
