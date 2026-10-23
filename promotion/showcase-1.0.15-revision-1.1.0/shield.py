"""File: shield.py
Description: Cover the desktop background during authorized video recording
Author: 5garashi.com Design Office
Created: 2026-10-20
Version: 1.1.0
License: MIT
SPDX-License-Identifier: MIT
"""
import tkinter as tk
r=tk.Tk();r.overrideredirect(True);r.geometry(f'{r.winfo_screenwidth()}x{r.winfo_screenheight()}+0+0');r.configure(bg='#F9EFCB');r.mainloop()
