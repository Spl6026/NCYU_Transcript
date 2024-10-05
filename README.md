# Transcript_dotnet 專案說明

Transcript_dotnet 是主程式，包含 API 與前端功能：

- **API**：生成學生的英文累計成績單及中文學期成績單報表。
- **前端**：有三個主要部分，存在**wwwroot**中：
  1. **inform**：根據 `webpid` 生成當學期成績單，供學生生成自己的成績單。
  2. **inform_button**：與 `inform` 相似，但技術實現不同，一個使用 React，另一個使用 JavaScript。
  3. **transen**：供教職員使用，根據 `webpid` 權限限縮生成成績單的系所學生範圍。

詳細內容可參考系統架構中的說明。

- **./inform_frontend**：為 `inform` React build 前的程式碼。
- **./transcript_frontend**：為 `transen` React build 前的程式碼。

## IIS 設置指引

1. 安裝 **.NET 6.0 Hosting Bundle**。
2. 將 `./Transcript_dotnet` 專案發布至所需的資料夾。
3. 將相依檔案 **`./img`**, **`./test.frx`**, **`./transcript.frx`** 一併放入發布資料夾中。

## 其他版本

- **net-framework branch**：為舊版本，已更新為 **netcore**。
