   M S B A M L   `   `   `  ����  Library!    Library.AuthorizationWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          � Title$7  1Вход в систему "БИБЛИОТЕКА"��5   	    � SizeToContent$ WidthAndHeight=�5   	    � WindowStartupLocation$ CenterScreen=�5	   	    � Icon$3 -Icon/icons8-логин-и-пароль-64.png��5
   	    � 
ResizeMode$ NoResize=�5   	   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $ ��Введите логин:��6!   �� 5   
   -   5        $��LoginTextBox��5      $��1��6   $	��250��63   6?   �� 5   
   $��2��6   $"��Введите пароль:��6!   G� 5   
   -   5        $��PasswordTextBox��5   #   $��3��6    G� PasswordChar$ *��6;   $	��250��6L   6X   � 5   
   $��4��6   $��Right=�6!   $��0,15,0,0q�6=   .+�5      �� -   5        $��LoginButton��5       �� 	IsDefault . $��60��6M   $��	0,0,130,0q�6[   $��
Войти��6n   {�.   ^� ShowOnDisabled . 	 � ToolTip$X	 RВведите логин и пароль, чтобы войти в систему��6�   �� 5      -   5        
 �� IsCancel
 . $��60��5   %   $��Отмена��63   $(	 "Выход из программы��6D   6a   5      5      5      