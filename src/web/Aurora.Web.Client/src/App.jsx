import { IssueExplorer } from "./Issues/IssueExplorer"
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"
import { RootLayout } from "./RootLayout/RootLayout"
import { NotFoundView } from "./RootLayout/NotFoundView"
import { CreateIssueView } from "./Issues/CreateIssueView"
import { IssueDetailsView } from "./Issues/IssueDetailsView"
import { SettingsView } from "./Settings/SettingsView"

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<RootLayout />}>
          <Route index element={<Navigate to="issue/explorer" replace />} />
          <Route path="issue/explorer" element={<IssueExplorer />} />
          <Route path="issue/create" element={<CreateIssueView />} />
          <Route path="issue/:issueId" element={<IssueDetailsView />} />
          <Route path="settings" element={<SettingsView />} />
          <Route path="*" element={<NotFoundView />} />
        </Route>
      </Routes>
    </BrowserRouter >
  )
}

export default App