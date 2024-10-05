import { Link, Outlet, useRouteError } from "react-router-dom";

export function RootLayout() {
    return (
        <>
            <div style={{ backgroundColor: "gray", padding: "1px"}}>
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

