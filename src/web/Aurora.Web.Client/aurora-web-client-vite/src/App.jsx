import { IssueExplorer } from "./Issues/IssueExplorer"
import { IssueDetails } from "./Issues/IssueDetails"
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"
import { RootLayout } from "./RootLayout/RootLayout"
import { NotFoundView } from "./RootLayout/NotFoundView"

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<RootLayout />}>
          <Route index element={<Navigate to="issue-explorer" replace />} />
          <Route path="issue-explorer" element={<IssueExplorer />} />
          <Route path="issue/:issueId" element={<IssueDetails />} />
          <Route path="*" element={<NotFoundView />} />
        </Route>
      </Routes>
    </BrowserRouter >
  )
}

export default App