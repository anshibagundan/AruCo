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

	// Quiz, Action, Difficulty系の初期化
	quizRepo := persistence.NewQuizRepository(db)
	actionRepo := persistence.NewActionRepository(db)
	quizUsecase := usecase.NewQuizUseCase(quizRepo)
	actionUsecase := usecase.NewActionUseCase(actionRepo)
	quizHandler := handlers.NewQuizHandler(quizUsecase)
	actionHandler := handlers.NewActionHandler(actionUsecase)
	difficultyUsecase := usecase.NewDifficultyUsecase(quizRepo, actionRepo)
	difficultyHandler := handlers.NewDifficultyWebSocketHandler(difficultyUsecase)

	// UserData系の初期化
	userDataRepo := persistence.NewUserDataPersistence(db)
	userDataUseCase := usecase.NewUserDataUsecase(
		quizRepo, userDataRepo,
		persistence.NewUserQuizResultRepository(db),
		persistence.NewUserActionResultRepository(db),
	)
	userDataHandler := handlers.NewUserDataHandler(userDataUseCase)

	// Result系の初期化
	resultHandler := handlers.NewResultWebSocketHandler(quizRepo, actionRepo, userDataRepo, userDataUseCase)

	// ScreenShare（Cast）系の初期化
	screenShareHandler := handlers.NewScreenShareHandler()

	r := mux.NewRouter()

	// UUIDエンドポイント
	r.HandleFunc("/createuuid", uuidHandler.CreateUUID).Methods("GET")
	r.HandleFunc("/getuuid", uuidHandler.GetUUID).Methods("GET")

	// WebSocketエンドポイント（Difficulty）
	r.HandleFunc("/ws/difficulty/android/{uuid}", difficultyHandler.HandleAndroidWebSocket)
	r.HandleFunc("/ws/difficulty/unity/{uuid}", difficultyHandler.HandleUnityWebSocket)

	// WebSocketエンドポイント（Result）
	r.HandleFunc("/ws/result/android/{uuid}", resultHandler.HandleResultAndroidWebSocket)
	r.HandleFunc("/ws/result/unity/{uuid}", resultHandler.HandleResultUnityWebSocket)

	// WebSocketエンドポイント（ScreenShare）
	r.HandleFunc("/ws/cast/android/{uuid}", screenShareHandler.HandleAndroidWebSocket)
	r.HandleFunc("/ws/cast/unity/{uuid}", screenShareHandler.HandleUnityWebSocket)

	// UserDataエンドポイント
	r.HandleFunc("/createuserdata", userDataHandler.CreateUserData).Methods("POST")
	r.HandleFunc("/getuserdata", userDataHandler.GetUserData).Methods("GET")

	// Quiz, Actionエンドポイント
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
