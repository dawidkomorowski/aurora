import { Link, Outlet, useRouteError } from "react-router-dom";

export function RootLayout() {
    return (
        <>
            <div style={{ backgroundColor: "gray", padding: "1px", display: "flex" }}>
                <div style={{ marginLeft: "20px" }}>
                    <h3>Aurora</h3>
                </div>
                <nav>
                    <ul>
                        <li>
                            <h3>
                                <Link to="/issue-explorer">Issue Explorer</Link>
                            </h3>
                        </li>
                    </ul>
                </nav>
            </div>
            <div>
                <Outlet />
            </div>
        </>
    );
}

