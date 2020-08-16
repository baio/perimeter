export interface UserRole {
    id: number;
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
