# TodoAI React

Frontend React cho TodoAI, giao tiếp với ASP.NET API.

## Setup

```bash
npm install
npm run dev
```

Mở http://localhost:5173

## Yêu cầu

ASP.NET backend phải đang chạy ở `https://localhost:7012`

Vite proxy tự động forward `/api/*` → `https://localhost:7012/api/*`
nên không cần lo CORS.

## API sử dụng

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | /api/todo | Lấy danh sách |
| POST | /api/todo | Tạo mới |
| DELETE | /api/todo/{id} | Xóa |
| PATCH | /api/todo/{id}/complete | Hoàn thành |
