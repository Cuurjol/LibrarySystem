   M S B A M L   `   `   `  ����  Library    Library.OrderDetailWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          � Title$  OrderDetailWindow��5   	   $	��310��5   	   $	��650��5	   	    � 
ResizeMode$ NoResize=�5
   	    � WindowStartupLocation$ CenterScreen=�5   	    � WindowStyle$ 
ToolWindow=�5   	    � 	Resources 5      &   System.Windows.Controls.DataGrid �� 
TargetType �� Property �� Value*  $System.Windows.Controls.DataGridCell �� 	Resources	 �� Color��%�  '        �� 5   
   "  6   .�5      ��  �@ 	CellStyle 5      �� 5      "  6   .�5      ��  � ��$ 0q�6<   �� 5       �@�FocusVisualStyle 6=   Z� 5      5      �� 5       � ��$	 200��6'   �� 5       �@ AutoGenerateColumns .  64   �� 5       �@ CanUserReorderColumns .  66   �� 5       �@ CanUserResizeColumns .  65   �� 5       �@ CanUserResizeRows .  62   �� 5       �@ CanUserAddRows .  6/   �� 5       �@ CanUserDeleteRows .  62   �� 5        �@ SelectionMode$ Single=�6.   �� 5!       �@ 
IsReadOnly . 6+   �� 5"      # �@ VerticalScrollBarVisibility$ Disabled=�6<   6O    5#      ���� 5$      $	 DarkBlue��6T   (��       6"   * SystemColors.HighlightBrushKey+)6T   �� 5%      $
	 Aqua��6X   (��       6"   *$"SystemColors.HighlightTextBrushKey+)6X   �� 5&      $	 	DarkGreen��6e   (��       6"   *1/SystemColors.InactiveSelectionHighlightBrushKey+)6e   �� 5'      $
	 Lime��6i   (��       6"   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)6i   5(      5)      5*      .��5+      � 
 � RowDefinitions
 5,   
   �� 5-      $
��Auto �6   �� 5.      $
��Auto �6   .+�50   
   �� $��0��6   .+�51      �� $��Читатель��6   $
��Left=�6(   $��10,5,0,0q�6C   $
��Wrap=�6U   $	��Top=�6i   � 52      .+�53      �� -   5        $��ReaderNameTextBox��53      $
��Left=�63   $��23��6N   $��10,5,0,0q�6Z   $��Center=�6l   $	��250��6�    �� 
IsReadOnly . 6�   �� 54      -   5        $��BlackListButton��54      $D��>Добавить читателя в чёрный список��60   $��Center=�6\   $��50,5,5,0q�6w   {�.  6�   55      56      �� 57   
   $��1��6   $��Center=�6    $��0,5,0,0q�6=   $	��Top=�6N   $	��620��6f   ��58      �� 59      $��Корзина книг��6   $
��Bold"�60   .��5;      �� .+�5<        -   5        $��BasketBookDataGrid��5<       5=         WrapText��%T   &	        �� 5>      " ��6    .�5?      ��  � ��$
 Wrap=�6=   5@      5A         Columns 5B      0  *System.Windows.Controls.DataGridTextColumn  5C      ,  &System.Windows.Controls.DataGridColumn   Header$ Название��6F      Width5  /System.Windows.Controls.DataGridLengthConverter$	 150 6X   1  +System.Windows.Controls.DataGridBoundColumn   ElementStyle# [  6d      Binding 6-   �� *Name+6d     5D      $ Жанр��6L   $	 120 6Z   # [  6f    6-   �� *
Genre.Name+6f     5E      $ 
Автор��6H   $	 120 6W   # [  6c    6-   �� *Author+6c     5F      $ Год издания��6F   $ 90 6[    6-   �� *Year+6[     5G      $ Каталог��6N   $	 120 6_   # [  6k    6-   �� *Catalog.Name+6k   5I      5J      5K      5L      5M      