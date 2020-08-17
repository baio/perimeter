export interface UserRole {
    id: string;
    email: string;
    roles: { id: number; name: string }[];
}

export interface User {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    roles: {
        id: number;
        name: string;
    }[];
}

export interface Permission {
    id: number;
    name: string;
    description: string;
}
