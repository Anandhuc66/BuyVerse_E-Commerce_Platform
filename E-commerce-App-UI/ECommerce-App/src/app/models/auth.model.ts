export interface Auth {
}
export interface RegisterModel {
  fullName: string;
  gender: string;
  email: string;
  password: string;
  phoneNumber?: string;
  role?: string; // optional, default 'User'
}

export interface LoginModel {
  email: string;
  password: string;
}

export interface UserResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  token: string;
}
