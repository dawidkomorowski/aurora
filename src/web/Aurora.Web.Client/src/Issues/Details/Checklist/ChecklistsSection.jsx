import { Checklist } from "./Checklist";

export function ChecklistsSection() {
    return (
        <div style={{ display: "flex", justifyContent: "center", marginTop: "20px" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }}>
                <div>
                    <strong>Checklists</strong>
                </div>
                <div style={{ marginTop: "10px" }}>
                    <Checklist checklistData={mockData[0]} />
                </div>
                <div style={{ marginTop: "10px" }}>
                    <Checklist checklistData={mockData[1]} />
                </div>
            </div>
        </div >
    );
}

const mockData = [
    {
        id: 1,
        title: "Checklist 1",
        items: [
            {
                id: 1,
                content: "First item is unchecked.",
                checked: false
            },
            {
                id: 2,
                content: "Second item is checked.",
                checked: true
            },
            {
                id: 3,
                content: "Third item is also unchecked.",
                checked: false
            }
        ]
    },
    {
        id: 2,
        title: "Acceptance criteria",
        items: [
            {
                id: 4,
                content: "Implement feature.",
                checked: true
            },
            {
                id: 5,
                content: "Add unit tests.",
                checked: true
            },
            {
                id: 6,
                content: "Merge to main branch.",
                checked: false
            },
            {
                id: 7,
                content: "Manual testing.",
                checked: false
            },
            {
                id: 8,
                content: "Deploy to DEV.",
                checked: false
            }
        ]
    }
]