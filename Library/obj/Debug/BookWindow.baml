   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  5   	    Library   Library.BookWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   X TXceed.Wpf.Toolkit, Version=3.2.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	   0sys&clr-namespace:System;assembly=mscorlib  5   	   -   5          � Title$  
Книга��5	   	   $	��450��5
   	   $	��300��5   	    � WindowStartupLocation$ CenterScreen=�5   	    � WindowStyle$ 
ToolWindow=�5   	    � 
ResizeMode$ NoResize=�5   	   .��5      � .+�5   
   �� $
��Left=�6   $��	10,10,0,0q�6/   $
��Wrap=�6B   $	��Top=�6V    � ToolTip$T NПоле является обязательным для заполнения��6n   �� 5      $�Название��6    �� 5      $�*��6   
���  ��6   5      �� 5   
   -   5        $��BookNameTextBox��5      $��Center=�6)   $��23��6F   $��0,30,0,0q�6R    �� TextWrapping$
 Wrap=�6d   $	��Top=�6x   $	��265��6�   �� 5   
   $
��Left=�6   $��	10,70,0,0q�6/   $
��Wrap=�6B   $	��Top=�6V   $T NПоле является обязательным для заполнения��6n   �� 5      $�Жанр��6    �� 5      $�*��6   
���  ��6   5      �� 5   
   -   5        $��BookGenreComboBox��5      $��Center=�6,   $��0,90,0,0q�6I   $	��Top=�6[   $	��265��6s    �� DisplayMemberPath$
 Name��6    �� SelectedValuePath$ Id��6�    �� 
IsEditable . 	 �� IsTextSearchEnabled	 .  6�   �� 5   
   $
��Left=�6   $��
10,130,0,0q�6/   $
��Wrap=�6C   $	��Top=�6W   $T NПоле является обязательным для заполнения��6o   �� 5      $�
Автор��6    �� 5      $�*��6   
���  ��6   5      �� 5   
   -   5        $��BookAuthorTextBox��5      $��Center=�6+   $��23��6H   $��	0,150,0,0q�6T   $
 Wrap=�6g   $	��Top=�6{   $	��265��6�   �� 5    
   $
��Left=�6   $��
10,190,0,0q�6/   $
��Wrap=�6C   $	��Top=�6W   $T NПоле является обязательным для заполнения��6o   �� 5!      $�Год издания��6    �� 5"      $�*��6   
���  ��6   5#      %  Xceed.Wpf.Toolkit.MaskedTextBox  5$   
   -   5        $��BookYearMaskedTextBox��5$      $��Center=�6:   $��23��6W   $��	0,210,0,0q�6c   $
 Wrap=�6v   $	��Top=�6�   $	��265��6�   
   Mask$

 0000��6�   4  .Xceed.Wpf.Toolkit.Primitives.ValueRangeTextBox   ValueDataType" G�6�   �� 5%   
   $
��Left=�6   $��
10,250,0,0q�6/   $
��Wrap=�6C   $��Каталог��6W   $	��Top=�6f   �� 5&   
   -   5        $��BookCatalogComboBox��5&      $��Center=�6.   $��	0,270,0,0q�6K   $	��Top=�6^   $	��265��6v   $
 Name��6�   $ Id��6�    . 	 .  6�   �� 5'   
   -   5        $��AddUpdateButton��5'      $��Добавить��6(   $��90��6;   $��10, 0, 0, 10q�6F   $
��Left=�6\   $��Bottom=�6w   6�   �� 5(   
   -   5        $��Отмена��5(      $��90��6"   $��0, 0, 10, 10q�6-   $��Right=�6C   $��Bottom=�6_   6z   5)      5*      